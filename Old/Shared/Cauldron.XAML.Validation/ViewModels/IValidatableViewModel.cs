﻿using Cauldron.Core;
using Cauldron.XAML.ViewModels;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace Cauldron.XAML.Validation.ViewModels
{
    /// <summary>
    /// Represents a viewmodel that is able to validate property values
    /// </summary>
    public interface IValidatableViewModel : IViewModel, INotifyDataErrorInfo, IDisposableObject
    {
        /// <summary>
        /// Starts a validation on all properties
        /// </summary>
        Task ValidateAsync();

        /// <summary>
        /// Starts a validation on a property defined by name.
        /// </summary>
        /// <param name="propertyName">The name of the property that requires validation</param>
        Task ValidateAsync(string propertyName);

        /// <summary>
        /// Starts a validation on a property defined by name.
        /// </summary>
        /// <param name="sender">The property info of the property that requested a validation</param>
        /// <param name="propertyName">The name of the property that requires validation</param>
        Task ValidateAsync(PropertyInfo sender, string propertyName);
    }
}