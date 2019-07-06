using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Net.Arqsoft.QsMapper.Model
{
    /// <summary>
    /// Base class for integer based tables.
    /// </summary>
    [Serializable]
    public abstract class Entity<T> : IEntity<T>
    {
        /// <summary>
        /// Use Id as HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Id);
        }

        /// <inheritdoc />
        public virtual T Id { get; set; }

        /// <inheritdoc />
        public abstract bool IsNew { get; }

        private string _name;

        /// <summary>
        /// Name (will be null if not contained in table).
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _findName = value?.ToLower();
            }
        }

        /// <summary>
        /// Common fields.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Common fields.
        /// </summary>
        public bool IsGhost { get; set; }

        /// <summary>
        /// Common fields.
        /// </summary>
        public DateTime? LastChange { get; set; }

        private string _findName;

        /// <summary>
        /// lower case version of name for find operations
        /// </summary>
        public string FindName => _findName;

        /// <summary>
        /// Override of == operator using interal Equals method.
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static bool operator ==(Entity<T> b1, Entity<T> b2)
        {
            return ReferenceEquals(null, b1) ? ReferenceEquals(null, b2) : b1.Equals(b2);
        }

        /// <summary>
        /// Override of != operator using interal Equals method.
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static bool operator !=(Entity<T> b1, Entity<T> b2)
        {
            return ReferenceEquals(null, b1) ? !ReferenceEquals(null, b2) : !b1.Equals(b2);
        }

        /// <summary>
        /// BaseEntities are equal when Type and Id are equal.
        /// If one Id is 0 objects can only be equal if ReferenceEquals returns true.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return ReferenceEquals(this, obj) || Equals(obj as Entity<T>);
        }

        /// <summary>
        /// BaseEntities are equal when Type and Id are equal.
        /// If one Id is 0 objects can only be equal if ReferenceEquals returns true.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Entity<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other.Id, null)) return true;
            return !IsNew && Equals(other.Id, Id);
        }

        /// <summary>
        /// Static check method.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool NullOrNew(Entity<T> item)
        {
            return item == null || ReferenceEquals(item.Id, null) || Equals(item.Id, 0);
        }

        /// <summary>
        /// Serialize this instance.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var strm = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(strm, this);
            var bytes = new byte[strm.Length];
            strm.Position = 0;
            strm.Read(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Create from serialized data.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T1 Create<T1>(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var strm = new MemoryStream(bytes);
            var formatter = new BinaryFormatter();
            var o = formatter.Deserialize(strm);
            return (T1)o;
        }

        /// <summary>
        /// Reads all Properties from another object into this instance
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        public void UpdateFrom(object item)
        {
            foreach (var prop in GetType().GetProperties().Where(x => x.GetSetMethod() != null))
            {
                var value = prop.GetValue(item);
                prop.SetValue(this, value);
            }
        }
    }
}