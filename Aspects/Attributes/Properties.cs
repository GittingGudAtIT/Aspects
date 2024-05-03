﻿using Aspects.Attributes.Base;
using Aspects.Attributes.Interfaces;
using System;
using System.ComponentModel;

namespace Aspects.Attributes
{
    /// <summary>
    /// Attribute for automatic <see cref="INotifyPropertyChanged.PropertyChanged"/> code generation.<br/>
    /// Creates public property code that is linked to the attributed field.
    /// Also adds <see cref="PropertyChangedEventHandler"/> PropertyChanged and <see cref="INotifyPropertyChanged"/> to the class if not exist.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class NotifyPropertyChangedAttribute : PropertyEventGenerationAttribute, INotifyPropertyChangedAttribute
    {
        /// <summary>
        /// Creates an instance of <see cref="NotifyPropertyChangedAttribute"/>.
        /// </summary>
        /// <param name="equalityCheck">When set to true, event will just be fired when the value wich is set does not equal the field value.</param>
        public NotifyPropertyChangedAttribute(bool equalityCheck = false) : base (equalityCheck) { }

        public override PropertyEvent PropertyEvent { get; } = PropertyEvent.Changed;
    }

    /// <summary>
    /// Attribute for automatic <see cref="INotifyPropertyChanging.PropertyChanging"/> code generation.<br/>
    /// Creates public property code that is linked to the attributed field.
    /// Also adds <see cref="PropertyChangingEventHandler"/> PropertyChanging and <see cref="INotifyPropertyChanging"/> to the class if not exist.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class NotifyPropertyChangingAttribute : PropertyEventGenerationAttribute, INotifyPropertyChangingAttribute
    {
        /// <summary>
        /// Creates an instance of <see cref="NotifyPropertyChangingAttribute"/>.
        /// </summary>
        /// <param name="equalityCheck">When set to true, event will just be fired when the value wich is set does not equal the field value.</param>
        public NotifyPropertyChangingAttribute(bool equalityCheck = false) : base(equalityCheck) { }

        public override PropertyEvent PropertyEvent { get; } = PropertyEvent.Changing;
    }
}
