using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace TimeClock
{
    class ResourceManager
    {
        static ResourceManager instance;

        static ResourceManager()
        {
            instance = new ResourceManager();
        }

        public static ResourceManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Loads bitmap from resources.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>Instance of Icon class.</returns>
        public Icon LoadBitmapAsIcon(string fileName)
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("TimeClock.Core.Images." + fileName))
            {
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    bitmap.MakeTransparent(Color.Magenta);

                    return Icon.FromHandle(bitmap.GetHicon());
                }
            }
        }

        /// <summary>
        /// Loads icon from resources.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Icon LoadIcon(string fileName)
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("TimeClock.Core.Images." + fileName))
            {
                return new Icon(stream);
            }
        }
    }
}
