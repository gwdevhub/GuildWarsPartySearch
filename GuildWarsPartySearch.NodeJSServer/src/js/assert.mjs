export function assert(boolFunc, assertionMessage = "Assertion failure") {
    if(!boolFunc) {
        throw new Error(assertionMessage)
    }
}