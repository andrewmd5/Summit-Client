using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CodeUSAClient.Properties
{
    [CompilerGenerated, DebuggerNonUserCode,
     GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal class Resources
    {
        private static CultureInfo resourceCulture;
        private static ResourceManager resourceMan;

        internal static Bitmap bg
        {
            get { return (Bitmap) ResourceManager.GetObject("bg", resourceCulture); }
        }

        internal static Bitmap bg1
        {
            get { return (Bitmap) ResourceManager.GetObject("bg1", resourceCulture); }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        internal static byte[] font
        {
            get { return (byte[]) ResourceManager.GetObject("font", resourceCulture); }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (ReferenceEquals(resourceMan, null))
                {
                    var manager = new ResourceManager("CodeUSAClient.Properties.Resources", typeof (Resources).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }
    }
}