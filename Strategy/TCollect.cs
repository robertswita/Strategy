/**********************************************************
Autor: Robert Świta
Politechnika Koszalińska
Katedra Systemów Multimedialnych i Sztucznej Inteligencji
***********************************************************/
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Strategy
{
    public interface ICollect
    {
        bool Remove(TCollectItem item);
        void Insert(int index, TCollectItem item);
        TCollectItem this[int index] { get; set; }
        int Count { get; }
        object Owner { get; }
    }

    [Serializable]
    public class TCollectItem
    {
        int index;
        public TCollectItem Prev { get { return Collect[index - 1]; } }
        public TCollectItem Next { get { return Collect[index + 1]; } }
        [NonSerialized]
        //ICollect collect;
        // Zauważmy, że zamiast procedur dodawania, usuwania i przesuwania elementów kolekcji
        // upublicznionymi składowymi klasy TCollectItem są jedynie property Collect i Index.
        public ICollect Collect;
        //{
        //    get { return collect; }
        //    set
        //    {
        //        if (collect != value)
        //        {
        //            if (collect != null)
        //                collect.Remove(this);
        //            collect = value;
        //            if (collect != null)
        //                collect.Insert(this, collect.Count);
        //        }
        //    }
        //}
        public int Index
        {
            get { return index; }
            set
            {
                if (index != value)
                {
                    Collect.Remove(this);
                    Collect.Insert(index, this);
                }
                index = value;
            }
        }
        internal void ReIndex(int index)
        {
            for (int i = index; i < Collect.Count; i++)
                Collect[i].index = i;
        }
        public TCollectItem Clone()
        {
            MemoryStream memS = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memS, this);
            memS.Position = 0;
            return (TCollectItem)bf.Deserialize(memS);
        }

        public virtual void Read(BinaryReader reader) { }
        public virtual void Write(BinaryWriter writer) { }
    };
    // Klasy TCollectItem<T> i TCollect<T> są typu generic (odpowiednik klas wzorcowych – templates w C++)
    // Kolekcje dziedziczące po TCollect<T> zawierają obiekty T, które dziedziczą po TCollectItem<T> (klauzula where).
    // Metoda Add tworzy nowy obiekt typu T.
    // W ten sposób będziemy dodawać elementy do kolekcji, zamiast jawnego konstruowania obiektów.
    [Serializable]
    public class TCollect<T> : Collection<T>, ICollect where T : TCollectItem, new()
    {
        public object Owner { get; set; }
        public Type ItemType = typeof(T);

        TCollectItem ICollect.this[int index]
        {
            get { return (T)this[index]; }
            set { this[index] = (T)value; }
        }
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            item.Collect = this;
            item.ReIndex(index);
        }
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            if (index < Count)
                this[index].ReIndex(index);
        }
        public bool Remove(TCollectItem item) { return base.Remove((T)item); }
        public void Insert(int index, TCollectItem item) { base.Insert(index, (T)item); }
        public T Add()
        {
            //T result = new T();
            var result = (T)Activator.CreateInstance(ItemType);
            Add(result);
            return result;
        }

        public TCollect<T> Clone()
        {
            var dest = new TCollect<T>();
            //dest.Capacity = Capacity;
            for (int i = 0; i < Count; i++)
            {
                T item = (T)this[i].Clone();
                //item.Collect = dest;
                dest.Add(item);
            }
            return dest;
        }
        public void Read(BinaryReader reader)
        {
            Clear();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                Add().Read(reader);
        }
        public void Write(BinaryWriter writer) 
        {
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
                this[i].Write(writer);
        }

        public void LoadFromFile(string fileName)
        {
            var reader = new BinaryReader(File.OpenRead(fileName));
            Read(reader);
            reader.Close();
        }
        public void SaveToFile(string fileName)
        {
            var writer = new BinaryWriter(File.OpenWrite(fileName));
            Write(writer);
            writer.Close();
        }
    };
}
