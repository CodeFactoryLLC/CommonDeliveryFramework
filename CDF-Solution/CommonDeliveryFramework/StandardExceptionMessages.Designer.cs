﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CommonDeliveryFramework {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class StandardExceptionMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal StandardExceptionMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CommonDeliveryFramework.StandardExceptionMessages", typeof(StandardExceptionMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A communication error has occured within the application..
        /// </summary>
        public static string CommunicationException {
            get {
                return ResourceManager.GetString("CommunicationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Configuration information for the application could not be found or is invalid..
        /// </summary>
        public static string ConfigurationException {
            get {
                return ResourceManager.GetString("ConfigurationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error has occured while processing data in the application..
        /// </summary>
        public static string DataException {
            get {
                return ResourceManager.GetString("DataException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not access external systems do to access being denied..
        /// </summary>
        public static string ExternalAccessException {
            get {
                return ResourceManager.GetString("ExternalAccessException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The current operation had an internal error..
        /// </summary>
        public static string LogicException {
            get {
                return ResourceManager.GetString("LogicException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An internal application error has occured..
        /// </summary>
        public static string ManagedException {
            get {
                return ResourceManager.GetString("ManagedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A security error has occured within the application..
        /// </summary>
        public static string SecurityException {
            get {
                return ResourceManager.GetString("SecurityException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The current operation has timed out and could not complete..
        /// </summary>
        public static string TimeoutException {
            get {
                return ResourceManager.GetString("TimeoutException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An internal application error has occured. Please confirm the operation completed successfully..
        /// </summary>
        public static string UnhandledException {
            get {
                return ResourceManager.GetString("UnhandledException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A validation error has occured in the application, required information is incorrect or missing..
        /// </summary>
        public static string ValidationException {
            get {
                return ResourceManager.GetString("ValidationException", resourceCulture);
            }
        }
    }
}
