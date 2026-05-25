using System;

namespace Howl;

/// <remarks>
///    <para>Remarks:</para>
///    <para>This is a dummy attribute to load the Howl assembly into memory.</para>
/// </remarks>
public class Dummy : Attribute{}

[AttributeUsage(AttributeTargets.Struct)]
public class Component : Attribute{}

[AttributeUsage(AttributeTargets.Field)]
public class SerialiseField : Attribute{}