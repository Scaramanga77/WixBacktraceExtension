﻿namespace WixBacktraceExtension
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using Backtrace;
    using Actions;
    using SitePublication;
    using Microsoft.Tools.WindowsInstallerXml;

    /// <summary>
    /// Backtrace extension interface
    /// </summary>
    public class BacktracePreprocessorExtension : PreprocessorExtension
    {
        private List<string> _componentsGenerated;
        public const string ComponentTemplate = @"<Component Id='{0}' Directory='{1}'><File Id='{2}' Source='{3}' KeyPath='yes'/></Component>";
        public override string[] Prefixes { get { return new[] { "publish", "build", "include" }; } }

        /// <summary>
        /// Prefixed variables, called like $(prefix.name)
        /// </summary>
        /// <param name="prefix">This is matched to <see cref="Prefixes"/> to find this plugin.</param>
        /// <param name="name">Name of the variable whose value is to be returned.</param>
        public override string GetVariableValue(string prefix, string name)
        {
            if (prefix != "publish" || name != "tempDirectory") return null; // making temp directories is all we do here.

            var target = Path.Combine(Path.GetTempPath(), "publish_" + Guid.NewGuid());
            Directory.CreateDirectory(target);

            return target;
        }

        /// <summary>
        /// The syntax is &lt;?pragma prefix.name args?&gt; where the arguments are just a string. Don't close the XmlWriter
        /// </summary>
        /// <param name="sourceLineNumbers"></param>
        /// <param name="prefix">The pragma prefix. This is matched to <see cref="Prefixes"/> to find this plugin. We have 'build' and 'include' for different sections of the .wxs</param>
        /// <param name="pragma">The command being requested. This is our dispatch key</param>
        /// <param name="args">Any arguments passed to the pragma. This is a raw string, but any $() references will have been resolved for us</param>
        /// <param name="writer">Output into the final .wxs XML file</param>
        /// <returns></returns>
        public override bool ProcessPragma(SourceLineNumberCollection sourceLineNumbers, string prefix, string pragma, string args, XmlWriter writer)
        {
            var cleanArgs = new QuotedArgsSplitter(args);
            switch (prefix)
            {
                case "build":
                    return BuildActions(pragma, cleanArgs, writer);

                case "include":
                    return PreprocessorActions.ReferenceComponents(pragma, cleanArgs, writer);

                case "publish":
                    return PublishCommands(pragma, cleanArgs, writer);

                default:
                    return false;
            }
        }

        bool BuildActions(string pragma, QuotedArgsSplitter cleanArgs, XmlWriter writer)
        {
            switch (pragma)
            {
                case "componentsFor":
                    return PreprocessorActions.BuildComponents(_componentsGenerated, cleanArgs, writer);

                case "transformConfigOf":
                    return PreprocessorActions.TransformConfiguration(cleanArgs, writer);

                case "directoriesMatching":
                    return PreprocessorActions.BuildMatchingDirectories(cleanArgs, writer);

                default:
                    return false;
            }
        }

        static bool PublishCommands(string command, QuotedArgsSplitter args, XmlWriter writer)
        {
            switch (command)
            {
                case "webSiteProject":
                    return Website.PublishSiteToFolder(args, writer);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Startup actions.
        /// </summary>
        public override void InitializePreprocess()
        {
            base.InitializePreprocess();
            _componentsGenerated = new List<string>();
        }

        /// <summary>
        /// Actions performed once XML has been finally generated, before WiX project is compiled
        /// </summary>
        public override void FinalizePreprocess()
        {
            base.FinalizePreprocess();
            _componentsGenerated.Clear();
        }
    }
}
