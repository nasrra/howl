using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Howl.Ecs;

public static class GenIndexResultExtensions
{
    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> is <see cref="GenIndexResult.Ok"/>.
    /// </summary>
    /// <param name="r">the <see cref="GenIndexResult"/></param>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the <see cref="GenIndexResult"/> is <see cref="GenIndexResult.Ok"/>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Ok(this GenIndexResult r, out GenIndexResult result)
    {
        result = r;
        return Ok(r);
    }

    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> is <see cref="GenIndexResult.Ok"/>.
    /// </summary>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the <see cref="GenIndexResult"/> is <see cref="GenIndexResult.Ok"/>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Ok(this GenIndexResult result)
    {
        return HandleOkOrFailResult(result, true);
    }

    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> is not <see cref="GenIndexResult.Ok"/>.
    /// </summary>
    /// <param name="r">the <see cref="GenIndexResult"/></param>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the result is not <see cref="GenIndexResult.Ok"/>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Fail(this GenIndexResult r, out GenIndexResult result)
    {
        result = r;
        return Fail(r);
    }

    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> is not <see cref="GenIndexResult.Ok"/>>
    /// </summary>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the result is not <see cref="GenIndexResult.Ok"/>; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Fail(this GenIndexResult result)
    {
        return HandleOkOrFailResult(result, false);
    }

    /// <summary>
    /// Returns the correct flag for Fail and Ok checks.
    /// </summary>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <param name="flag">the flag to return if the <see cref="GenIndexResult"/> is the <see cref="GenIndexResult.Ok"/></param>
    /// <returns>the flag.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool HandleOkOrFailResult(GenIndexResult result, bool flag)
    {        
        switch (result)
        {
            case GenIndexResult.Ok:
                return flag;

            case GenIndexResult.DenseNotAllocated:
            case GenIndexResult.StaleGenIndex:
                return !flag;

            case GenIndexResult.DenseAlreadyAllocated:
            case GenIndexResult.InvalidGenIndex:
            case GenIndexResult.StaleDenseAllocation:
                return !flag;


            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }        
    }

    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> successfully a rolledback.
    /// </summary>
    /// <param name="r">the <see cref="GenIndexResult"/></param>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the rollback is successful; otherwise false.</returns>
    public static bool RollbackOk(this GenIndexResult r, out GenIndexResult result)
    {
        result = r;
        return RollbackOk(r);
    }


    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> successfully a rolledback.
    /// </summary>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the rollback is successful; otherwise false.</returns>
    public static bool RollbackOk(this GenIndexResult result)
    {
        return HandleRollbackResult(result, true);
    }

    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> has failed a rollback.
    /// </summary>
    /// <param name="r">the <see cref="GenIndexResult"/></param>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the rollback is successful; otherwise false.</returns>
    public static bool RollbackFail(this GenIndexResult r, out GenIndexResult result)
    {
        result = r;
        return RollbackFail(r);
    }

    /// <summary>
    /// Checks that a <see cref="GenIndexResult"/> has failed a rollback.
    /// </summary>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <returns>true, if the rollback is successful; otherwise false.</returns>
    public static bool RollbackFail(this GenIndexResult result)
    {
        return HandleRollbackResult(result, false);
    }

    /// <summary>
    /// Returns the correct result for rollback checks.
    /// </summary>
    /// <param name="result">the <see cref="GenIndexResult"/></param>
    /// <param name="flag">the flag to return if the <see cref="GenIndexResult"/> is a successful rollback.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static bool HandleRollbackResult(GenIndexResult result, bool flag)
    {
        switch (result)
        {
            case GenIndexResult.Ok:
            case GenIndexResult.DenseNotAllocated:
                return flag;

            case GenIndexResult.StaleGenIndex:
                return !flag;

            case GenIndexResult.DenseAlreadyAllocated:
            case GenIndexResult.InvalidGenIndex:
            case GenIndexResult.StaleDenseAllocation:
                Debug.Assert(false, $"Unexpected GenIndexResult: {result}");
                return !flag;


            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);            
        }
    }
}