using System;
using System.Collections.Generic;
using System.Text;

namespace SadConsole.Consoles
{
    public class LayerMetadata
    {
        string Name;

        public static LayerMetadata Create(string name, LayeredTextSurface.Layer layer)
        {
            LayerMetadata meta = new LayerMetadata();
            meta.Name = name;
            layer.Metadata = meta;
            return meta;
        }
    }
}
