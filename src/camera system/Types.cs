using System;
using N_Howl.N_Collections;
using N_Howl.N_Math;

namespace N_Howl.N_CameraSystem;

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>member values shouldn't be directly modified without good reason; refer to <see cref="CameraSystem"/>.</para>
/// </remarks>
public struct Camera{
    public Vector3 Position;
    public Matrix4x4 Projection;
    public Matrix4x4 View;
    public Matrix4x4 Model;
    public float OrthographicSize;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>This value is in radians.</para>
    /// </remarks>
    public float PerspectiveFov;
    public float FarZ;
    public float NearZ;
    public ProjectionType ProjectionType;
    public bool IsInitialised;
}

public enum ProjectionType{
    Orthographic,
    Perspective
}