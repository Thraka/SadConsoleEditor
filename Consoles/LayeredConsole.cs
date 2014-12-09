using System;
using Console = SadConsole.Consoles.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Consoles;
using SadConsole;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using SadConsole.Input;

namespace SadConsoleEditor.Consoles
{
    [DataContract]
    public class LayeredConsole: Console
    {
        [DataMember]
        public int Width { get; protected set; }
        [DataMember]
        public int Height { get; protected set; }

        public int Layers { get { return _layers.Count; } }

        [IgnoreDataMember]
        public CellSurface ActiveLayer { get; protected set; }

        [DataMember(Name = "LayerNames")]
        private List<string> _layerNames;

        [DataMember(Name = "Layers")]
        protected List<CellsRenderer> _layers;

        public CellsRenderer this[int index]
        {
            get { return _layers[index]; }
        }

        public LayeredConsole(int layers, int width, int height): base(width, height)
        {
            Width = width;
            Height = height;

            _layers = new List<CellsRenderer>();
            _layerNames = new List<string>();

            for (int i = 0; i < layers; i++)
                AddLayer(i.ToString());


            SetActiveLayer(0);
        }

        public void Clear(Color foreground, Color background)
        {
            // Create all layers
            for (int i = 0; i < _layers.Count; i++)
            {
                _layers[i].CellData.DefaultBackground = background;
                _layers[i].CellData.DefaultForeground = foreground;
                _layers[i].CellData.Clear();
            }
        }

        public void SetActiveLayer(int index)
        {
            if (index < 0 || index > _layers.Count - 1)
                throw new ArgumentOutOfRangeException("index");

            _cellData = _layers[index].CellData;
            ActiveLayer = _cellData;
            ResetViewArea();
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            for (int i = 0; i < _layers.Count; i++)
                _layers[i].CellData.Resize(width, height);

            ResetViewArea();
        }

        public void Move(Point position)
        {
            this.Position = position;

            for (int i = 0; i < _layers.Count; i++)
                _layers[i].Position = position;
        }

        public override void Update()
        {
            for (int i = 0; i < _layers.Count; i++)
                _layers[i].Update();
        }

        public override void Render()
        {
            for (int i = 0; i < _layers.Count; i++)
                _layers[i].Render();
        }

        public void SetLayerName(int layer, string name)
        {
            _layerNames[layer] = name;
        }

        public string GetLayerName(int layer)
        {
            return _layerNames[layer];
        }

        public void RemoveLayer(int layer)
        {
            _layers.RemoveAt(layer);
            _layerNames.RemoveAt(layer);
        }

        public void AddLayer(string name)
        {
            _layers.Add(new CellsRenderer(new CellSurface(Width, Height), Batch));
            _layerNames.Add(name);
        }

        public void InsertLayer(int index)
        {
            _layers.Insert(index, new CellsRenderer(new CellSurface(Width, Height), Batch));
            _layerNames.Insert(index, "");
        }

        public void MoveLayer(int index, int newIndex)
        {
            var layer = _layers[index];
            var layerName = _layerNames[index];

            _layers.Insert(newIndex, layer);
            _layerNames.Insert(newIndex, layerName);

            _layers.RemoveAt(index);
            _layerNames.RemoveAt(index);
        }

        public IEnumerable<CellsRenderer> GetEnumeratorForLayers()
        {
            return _layers;
        }

        [OnDeserialized]
        private void AfterDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            SetActiveLayer(0);
        }
        private CellSurface _tempSurface;
        [OnSerializing]
        private void BeforeSerializing(StreamingContext context)
        {
            _tempSurface = _cellData;
            _cellData = null;
        }

        [OnSerialized]
        private void AfterSerialized(StreamingContext context)
        {
            _cellData = _tempSurface;
            _tempSurface = null;
        }
    }


}
