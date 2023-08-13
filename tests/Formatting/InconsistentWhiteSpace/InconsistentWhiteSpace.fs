module   Fake

open     System

let    add   ( birthDate  :     DateTime) =
    birthDate.Add (    TimeSpan.FromSeconds  (   1000000000.0   ) )
