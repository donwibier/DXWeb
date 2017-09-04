////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	collection.cs
//
// summary:	Implements the collection class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DX.Utils
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A collection item. </summary>
    ///
    /// <remarks>   Don, 3-2-2016. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    [TypeConverter(typeof(CollectionItemConverter))]
    public class CollectionItem : IDisposable
        
    {
        /// <summary>   The collection. </summary>
        internal BaseCollection collection;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Default constructor. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public CollectionItem()
            : base()
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the collection. </summary>
        ///
        /// <value> The collection. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [Browsable(false)]
        public BaseCollection Collection
        {
            get { return collection; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Releases the unmanaged resources used by the DX.Utils.CollectionItem and optionally releases
        /// the managed resources.
        /// </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="disposing">    true to release both managed and unmanaged resources; false to
        ///                             release only unmanaged resources. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (collection != null)                    
                    collection.RemoveItem(this);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Finaliser. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ~CollectionItem()
        {
            Dispose(false);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Collection of bases. </summary>
    ///
    /// <remarks>   Don, 3-2-2016. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public abstract class BaseCollection : CollectionBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Ensures a CollectionItem only belongs to a single collection. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="item"> The item. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected void SetCollection(CollectionItem item)
        {
            if (item.collection != null && item.collection != this)
                item.collection.RemoveItem(item);
            item.collection = this;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Ensures an item is not null. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are
        ///                                             null. </exception>
        ///
        /// <param name="item"> The item. </param>
        ///
        /// <returns>   A CollectionItem. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected CollectionItem EnsureItemNotNull(object item)
        {
            if (!(item is CollectionItem))
                throw new ArgumentNullException("item cannot be null and must be of type CollectionItem");
            
            return (CollectionItem)item;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Removes the item described by item. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="item"> The item. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal void RemoveItem(CollectionItem item)
        {
            if (item.collection == this)
                item.collection = null;

            List.Remove(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the insert action. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="index">    Zero-based index of the. </param>
        /// <param name="value">    The value. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnInsert(int index, object value)
        {
            base.OnInsert(index, value);
            CollectionItem item = EnsureItemNotNull(value);
            SetCollection(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the clear action. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnClear()
        {
            foreach (object item in this.List)
            {
                if (item is CollectionItem)
                    ((CollectionItem)item).collection = null;
            }
            base.List.Clear();
            base.OnClear();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the remove action. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="index">    Zero-based index of the. </param>
        /// <param name="value">    The value. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnRemove(int index, object value)
        {
            base.OnRemove(index, value);
            CollectionItem item = EnsureItemNotNull(value);
            if (item.collection == this)
                item.collection = null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the set action. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="index">    Zero-based index of the. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> The new value. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            base.OnSet(index, oldValue, newValue);
            CollectionItem item = null;
            if (oldValue is CollectionItem)
            {
                item = (CollectionItem)oldValue;
                if (item.collection == this)
                    item.collection = null;
            }
            item = EnsureItemNotNull(newValue);
            base.List[index] = item;
            SetCollection(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the validate action. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values. </exception>
        ///
        /// <param name="value">    The value. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (!(value is CollectionItem))
                throw new ArgumentException("value must be of type CollectionItem", "value");
            
        }
    }
    
    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A collection. </summary>
    ///
    /// <remarks>   Don, 3-2-2016. </remarks>
    ///
    /// <typeparam name="TItemClass">   Type of the item class. </typeparam>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Collection<TItemClass> : BaseCollection, IDisposable
        where TItemClass : CollectionItem
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the validate action. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values. </exception>
        ///
        /// <param name="value">    The value. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (!(value is TItemClass))
                throw new ArgumentException("value must be of type " + typeof(TItemClass).Name, "value");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Now the strongly typed methods. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="item"> The item. </param>
        ///
        /// <returns>   An Int32. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Int32 Add(TItemClass item)
        {
            return List.Add(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Inserts. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="index">    Zero-based index of the. </param>
        /// <param name="item">     The item. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Insert(Int32 index, TItemClass item)
        {
            List.Insert(index, item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Removes the given item. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="item"> The item. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Remove(TItemClass item)
        {
            List.Remove(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Clears this object to its blank/initial state. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public new void Clear()
        {
            List.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if this object contains the given item. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="item"> The item. </param>
        ///
        /// <returns>   true if the object is in this collection, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Boolean Contains(TItemClass item)
        {
            return List.Contains(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Index of the given item. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="item"> The item. </param>
        ///
        /// <returns>   An Int32. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Int32 IndexOf(TItemClass item)
        {
            return List.IndexOf(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Copies to. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="array">    The array. </param>
        /// <param name="index">    Zero-based index of the. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void CopyTo(TItemClass[] array, Int32 index)
        {
            List.CopyTo(array, index);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Declare a default property of this class. </summary>
        ///
        /// <param name="index">    Zero-based index of the entry to access. </param>
        ///
        /// <returns>   The indexed item. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public virtual TItemClass this[Int32 index]
        {
            get { return (TItemClass)List[index]; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Finally implement Idisposable. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Releases the unmanaged resources used by the DX.Utils.Collection&lt;TItemClass&gt; and
        /// optionally releases the managed resources.
        /// </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="disposing">    true to release both managed and unmanaged resources; false to
        ///                             release only unmanaged resources. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (CollectionItem item in List)
                    ((IDisposable)item).Dispose();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Finaliser. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ~Collection()
        {
            Dispose(false);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A collection item converter. </summary>
    ///
    /// <remarks>   Don, 3-2-2016. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    internal class CollectionItemConverter : TypeConverter
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// If asked if we can convert to an InstanceDescriptor then return true. Otherwise ask our
        /// base class.
        /// </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="context">          The context. </param>
        /// <param name="destinationType">  Type of the destination. </param>
        ///
        /// <returns>   true if we can convert to, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;
            return
              base.CanConvertTo(context, destinationType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Our converter is capable of performing the conversion. </summary>
        ///
        /// <remarks>   Don, 3-2-2016. </remarks>
        ///
        /// <param name="context">          The context. </param>
        /// <param name="culture">          The culture. </param>
        /// <param name="value">            The value. </param>
        /// <param name="destinationType">  Type of the destination. </param>
        ///
        /// <returns>   to converted. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                Type valueType = value.GetType();
                ConstructorInfo ci = valueType.GetConstructor(System.Type.EmptyTypes);
                return new InstanceDescriptor(ci, null, false);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}