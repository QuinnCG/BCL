# BCL
A compiler for "Basic Command Language".

Basic Command Language is a personel experiment for generating IL code. It's not feature complete and provides terrible error messages.
It was initially made in one afternoon; though there may be some fixes here and there.

BCL Example
```bcl
var int x 5
var int y 7
var int result

call add x y result
call print result
```

Types:
- int
- long
- float
- double
- string

Commands:
- var [type] [name] [optiona-value]
- call [func-name] [func-arg-1] [func-arg-2] [etc.]

Functions:
- print [variable]
- add [varA] [varB] [var-to-store-result]
- subtract [varA] [varB] [var-to-store-result]
- multiply [varA] [varB] [var-to-store-result]
- divide [varA] [varB] [var-to-store-result]
- read [var-to-store-result]
- goto [label]
- goto [label] [varA] [comparison-operator] [varB]
- pause

Comparison Operators:
- =: equals.
- !: not equals.
- \>=: greater or equal.
- <=: less or equal.
- \>: greater.
- <: less.

## Pogram Examples

Hello World
```bcl
var string msg Hello World!
call print msg
```

Math
```bcl
var int x 5
var int y 7

var int sum
var int difference
var int product
var int remainder

call add x y sum
call subtract x y difference
call multiply x y product
call divide x y remainder

call print sum
call print difference
call print product
call print remainder
```

For Loop
```bcl
var int index 0
var int max 10
var int one 1

label start
call print index
call add index one index
call goto start index < max
```

If Statement
```bcl
var int x 5
var int y 5

call goto true x = y
call goto false

label true
var string msgTrue True!
call print msgTrue
call goto end

label false
var string msgFalse False!
call print msgFalse

label end
```
