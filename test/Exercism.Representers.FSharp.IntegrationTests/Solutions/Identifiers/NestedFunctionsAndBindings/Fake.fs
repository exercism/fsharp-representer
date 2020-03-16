module Fake

let sum x y = 
    let total = x + y
    total

let sub a = 
    let inner b = a - b
    inner

let mul = 
    let inner x = 
        let inner' y = x * y
        inner'

    inner
