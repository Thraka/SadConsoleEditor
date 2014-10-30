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

namespace SadConsoleEditor.Consoles
{
    [DataContract]
    class LayeredConsole: Console
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public int Layers { get; protected set; }

        [IgnoreDataMember]
        public CellSurface ActiveLayer { get; protected set; }

        [DataMember(Name = "Layers")]
        protected CellsRenderer[] _layers;

        public CellsRenderer this[int index]
        {
            get { return _layers[index]; }
        }

        public LayeredConsole(int layers, int width, int height): base(width, height)
        {
            Width = width;
            Height = height;
            Layers = layers;

            _layers = new CellsRenderer[layers];

            // Create all layers
            for (int i = 0; i < layers; i++)
            {
                _layers[i] = new CellsRenderer(new CellSurface(width, height), this.Batch);
            }

            SetActiveLayer(0);
        }

        public void SetActiveLayer(int index)
        {
            if (index < 0 || index > _layers.Length - 1)
                throw new ArgumentOutOfRangeException("index");

            _cellData = _layers[index].CellData;
            ActiveLayer = _cellData;
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            for (int i = 0; i < _layers.Length; i++)
                _layers[i].CellData.Resize(width, height);
        }

        public void Move(Point position)
        {
            this.Position = position;

            for (int i = 0; i < _layers.Length; i++)
                _layers[i].Position = position;
        }

        public override void Update()
        {
            for (int i = 0; i < _layers.Length; i++)
                _layers[i].Update();
        }

        public override void Render()
        {
            for (int i = 0; i < _layers.Length; i++)
                _layers[i].Render();
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
