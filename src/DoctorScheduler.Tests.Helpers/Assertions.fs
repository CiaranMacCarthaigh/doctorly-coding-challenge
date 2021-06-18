namespace DoctorScheduler.Tests.Helpers

open NUnit.Framework

module Assertions =
    let shouldEqual<'T> (expected: 'T) (actual: 'T) =
        Assert.That(actual, Is.EqualTo(expected))

    let shouldBeOkAndEqualTo (expected: Result<_, _>) (actual: Result<_, _>) =
        match expected, actual with
        | Ok expectedOk , Ok actualOk -> actualOk |> shouldEqual expectedOk
        | _ , _  ->
            sprintf "Expected inputs to be Ok but:\n Expected: %A\n Actual: %A" expected actual
            |> Assert.Fail

    let shouldBeErrorAndEqualTo (expected: Result<_, _>) (actual: Result<_, _>) =
        match expected, actual with
        | Error expectedError , Error actualError -> actualError |> shouldEqual expectedError
        | _ , _  ->
            sprintf "Expected inputs to be Error but:\n Expected: %A\n Actual: %A" expected actual
            |> Assert.Fail
