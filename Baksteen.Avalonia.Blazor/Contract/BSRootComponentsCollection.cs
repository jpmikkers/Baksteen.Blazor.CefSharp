// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Web;
using System.Collections.ObjectModel;

namespace Baksteen.Avalonia.Blazor.Contract;

/// <summary>
/// A collection of <see cref="BSRootComponent"/> items.
/// </summary>
public class BSRootComponentsCollection : ObservableCollection<BSRootComponent>, IJSComponentConfiguration
{
    /// <inheritdoc />
    public JSComponentConfigurationStore JSComponents { get; } = new();
}
