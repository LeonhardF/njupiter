#region Copyright & License
/*
	Copyright (c) 2005-2011 nJupiter

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/
#endregion
	/*

using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Net;

using log4net;

namespace nJupiter.Configuration {
	/// <summary>
	/// The configurator class to initialize config objects
	/// </summary>
	public static class Configurator {
		
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


		#region Static Methods

		/// <summary>
		/// Initialize a <see cref="Config" /> object for the current calling assembly that is using the provided <see cref="XmlElement" /> as it's configuration.
		/// </summary>
		/// <param name="element">The Xml element contining the configuration for the current calling assembly.</param>
		public static void Configure(XmlElement element) {
			Configure(Assembly.GetCallingAssembly(), element);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for a given assembly that is using the provided <see cref="XmlElement" /> as it's configuration.
		/// </summary>
		/// <param name="assembly">The assembly to configure.</param>
		/// <param name="element">The Xml element contining the configuration for the given assembly.</param>
		public static void Configure(Assembly assembly, XmlElement element) {
			if(assembly == null)
				throw new ArgumentNullException("assembly");
			Configure(assembly.GetName().Name, element);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for a configuration with the given config key using the provided <see cref="XmlElement" /> as it's configuration.
		/// </summary>
		/// <param name="configKey">The config key.</param>
		/// <param name="element">The Xml element contining the configuration with the given config key.</param>
		public static void Configure(string configKey, XmlElement element) {
			ConfigureFromXml(configKey, element);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for the current calling assembly that is using the provided <see cref="FileInfo" /> as it's configuration.
		/// </summary>
		/// <param name="configFile">The file containing the Xml that holds the configuration.</param>
		public static void Configure(FileInfo configFile) {
			Configure(Assembly.GetCallingAssembly(), configFile);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for the current calling assembly that is using the provided <see cref="Uri" /> as it's configuration.
		/// </summary>
		/// <param name="configUri">The Uri containing the Xml that holds the configuration.</param>
		public static void Configure(Uri configUri) {
			Configure(Assembly.GetCallingAssembly(), configUri);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for the current calling assembly that is using the provided <see cref="Stream" /> as it's configuration.
		/// </summary>
		/// <param name="configStream">The stream containing the Xml that holds the configuration.</param>
		public static void Configure(Stream configStream) {
			Configure(Assembly.GetCallingAssembly(), configStream);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for a sprecified assembly that is using the provided <see cref="FileInfo" /> as it's configuration.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="configFile">The file containing the Xml that holds the configuration.</param>
		public static void Configure(Assembly assembly, FileInfo configFile) {
			if(assembly == null)
				throw new ArgumentNullException("assembly");
			Configure(assembly.GetName().Name, configFile);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for  sprecified assembly that is using the provided <see cref="Uri" /> as it's configuration.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="configUri">The Uri containing the Xml that holds the configuration.</param>
		public static void Configure(Assembly assembly, Uri configUri) {
			if(assembly == null)
				throw new ArgumentNullException("assembly");
			Configure(assembly.GetName().Name, configUri);
		}

		/// <summary>
		/// Initialize a <see cref="Config" /> object for a configuration with the given config key using the provided <see cref="Uri" /> as it's configuration.
		/// </summary>
		/// <param name="configKey">The config key.</param>
		/// <param name="configUri">The Uri containing the Xml that holds the configuration.</param>
		public static void Configure(string configKey, Uri configUri) {
			if(configUri == null) {
				throw new ArgumentNullException("configUri");
			}
			if(configUri.IsFile) {
				// If URI is local file then call Configure with FileInfo
				Configure(configKey, new FileInfo(configUri.LocalPath));
			} else {
				WebRequest configRequest;

				try {
					configRequest = WebRequest.Create(configUri);
				} catch(Exception ex) {
					throw new ConfiguratorException(string.Format("Failed to create WebRequest for URI [{0}].", configUri), ex);
				}

				// authentication may be required, set client to use default credentials
				try {
					configRequest.Credentials = CredentialCache.DefaultCredentials;
					// ReSharper disable EmptyGeneralCatchClause
				} catch {
					// ReSharper restore EmptyGeneralCatchClause
					// ignore security exception
				}

				try {
					WebResponse response = configRequest.GetResponse();
					if(response != null) {
						try {
							// Open stream on config URI
							using(Stream configStream = response.GetResponseStream()) {
								XmlElement xmlElement = GetConfigXmlElement(configKey, configStream);
								IConfigSource source = ConfigSourceFactory.GetInstance().CreateConfigSource(configUri);
								Config config = new Config(configKey, xmlElement, source);
								SetConfig(config);
								
							}
						} finally {
							response.Close();
						}
					}
				} catch(Exception ex) {
					throw new ConfiguratorException(string.Format("Failed to request config from URI [{0}].", configUri), ex);
				}
			}
		}

		/// <summary>
		/// Initialize a <see cref="Config"/> object for a given assembly that is using the provided <see cref="Stream"/> as it's configuration.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="configStream">The stream containing the Xml that holds the configuration.</param>
		public static void Configure(Assembly assembly, Stream configStream) {
			if(assembly == null)
				throw new ArgumentNullException("assembly");
			Configure(assembly.GetName().Name, configStream);
		}

		/// <summary>
		/// Initialize a <see cref="Config"/> object for a configuration with the given config key using the provided <see cref="Stream"/> as it's configuration.
		/// </summary>
		/// <param name="configKey">The config key.</param>
		/// <param name="configStream">The stream containing the Xml that holds the configuration.</param>
		public static void Configure(string configKey, Stream configStream) {
			XmlElement xmlElement = GetConfigXmlElement(configKey, configStream);
			Config config = new Config(configKey, xmlElement);
			SetConfig(config);
		}

		/// <summary>
		/// Adds a <see cref="Config" /> object to the <see cref="ConfigHandler" />'s cache and also add a file watch to watch for changes if the config object is associated with a local file.
		/// </summary>
		/// <param name="config">The config object to configure.</param>
		public static void Configure(IConfig config) {
			SetConfig(config);
		}

		private static XmlElement GetConfigXmlElement(string conifgKey, Stream configStream) {
			if(configStream == null) {
				throw new ArgumentNullException("configStream");
			}
			// Load the config file into a document
			XmlDocument doc = new XmlDocument();
			try {
				// Create a validating reader around a text reader for the file stream
				XmlReaderSettings readerSettings = new XmlReaderSettings();
				XmlTextReader xtr = new XmlTextReader(configStream);
				// Specify that the reader should not perform validation, but that it should expand entity refs.
				readerSettings.ValidationType = ValidationType.None;
				xtr.EntityHandling = EntityHandling.ExpandEntities;
				XmlReader xmlReader = XmlReader.Create(xtr, readerSettings);
				// load the data into the document
				doc.Load(xmlReader);
			} catch(Exception ex) {
				if(!string.IsNullOrEmpty(conifgKey))
					throw new ConfiguratorException(string.Format("Error while loading XML configuration for the config with key [{0}].", conifgKey), ex);
				throw new ConfiguratorException("Error while loading XML configuration.", ex);
			}
			return GetXmlElementFromXmlNode(doc.DocumentElement);
		}

		private static void ConfigureFromXml(string configKey, XmlElement element) {
			XmlElement xmlElement = GetXmlElementFromXmlNode(element);	
			Config config = new Config(configKey, xmlElement);
			SetConfig(config);
		}

		private static XmlElement GetXmlElementFromXmlNode(XmlNode element) {
			if(element == null)
				throw new ArgumentNullException("element");
			// Copy the xml data into the root of a new document
			// this isolates the xml config data from the rest of  the document
			XmlDocument newDoc = new XmlDocument();
			XmlElement newElement = (XmlElement)newDoc.AppendChild(newDoc.ImportNode(element, true));

			return newElement;
		}

		internal static void SetConfig(IConfig config) {
			AddWatcherToFileConfig(config);
			ConfigHandler.Configurations.Insert(config);
		}

		private static void AddWatcherToFileConfig(IConfig config) {
			FileInfo configFile = config.ConfigSource.GetConfigSource<FileInfo>();
			if(configFile != null) {
				try {
					// Create a watch handler that will reload the configuration whenever the config file is modified.
					if(Log.IsDebugEnabled) { Log.Debug(string.Format("Start watching file {0} for config [{1}]", configFile.FullName, config.ConfigKey)); }
					WatchedConfigHandler wather = WatchedConfigHandler.StartWatching(config.ConfigKey, configFile);
					config.Disposed += wather.Disposing;
					
				} catch(Exception ex) {
					throw new ConfiguratorException(string.Format("Failed to initialize configuration file watcher for file [{0}].",configFile.FullName), ex);
				}
			}
		}
		#endregion
	}
}

	*/