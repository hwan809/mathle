**Project Title**: Mathle

**Overview**: Mathle is a console-based game implemented in F# that combines the mechanics of Wordle with mathematical equation guessing. The player is given a hidden equation and must identify it within 6 attempts. After each guess, the game provides symbol-coded feedback indicating whether each token is correct, misplaced, or absent — identical to the feedback system used in Wordle. The game runs entirely in the terminal with no external dependencies beyond .NET 10.

**Requirements**:

1. On startup, the game selects a hidden answer equation at random from a pre-generated pool of valid equations of the form `A op B = C`, where A and B are single-digit integers (1–9), op is one of `+`, `-`, `*`, `/`, and C is a positive integer result in the range 1–81. Equations where C would be zero or negative are excluded from the pool.

2. On startup, the game displays a welcome message and explains the feedback symbols: `G` (correct position), `Y` (exists but wrong position), `X` (not in the equation).

3. Before each input prompt, the game displays the current attempt number (e.g., `Attempt 1/6:`).

4. The player enters a guess as an equation string (e.g., `3+2=5` or `3 + 2 = 5`). Both compact and space-separated forms are accepted.

5. If the input cannot be parsed into the 5-token format `[number, operator, number, =, number]`, the game prints an error message and re-prompts the player. This attempt is **not** counted toward the 6-attempt limit.

6. If the input is syntactically valid but the left-hand side does not equal the right-hand side (e.g., `2+2=5`), the game prints an error message and re-prompts the player. This attempt is also **not** counted.

7. Division answers are only selected if the result is a whole number (no remainder). For example, `8/2=4` is a valid answer but `7/2=3` is not used.

8. If the guess is a valid equation, the game prints one line of feedback using the following symbols for each of the 5 tokens:
   - `G` — token is correct and in the correct position
   - `Y` — token exists in the answer but is in the wrong position
   - `X` — token does not appear in the answer at all

9. Feedback for duplicate tokens follows these rules: `G` is assigned first to all tokens in the correct position. Among the remaining non-`G` tokens, `Y` is assigned only as many times as the token still appears unmatched in the answer. Any excess occurrences receive `X`. For example, if the answer is `2+3=5` and the guess is `5+3=5`, feedback is computed as follows: position 1 (`5` vs `2`) — `5` is not in the answer at an unmatched position after accounting for position 5, so `X`; position 2 (`+` vs `+`) — `G`; position 3 (`3` vs `3`) — `G`; position 4 (`=` vs `=`) — `G`; position 5 (`5` vs `5`) — `G`. Result: `X G G G G`.

10. If all 5 tokens are `G`, the game prints a win message that includes the number of attempts used (e.g., `Correct! You solved it in 3 attempt(s).`) and exits.

11. If 6 valid guesses have been made without all tokens being `G`, the game prints a loss message, reveals the answer (e.g., `Game over. The answer was: 1+4=5`), and exits.

**Example Interaction**:

```
=== MATHLE ===
Guess the hidden equation (A op B = C) in 6 tries.
Feedback: G = right position, Y = wrong position, X = not present

Attempt 1/6: 3+4=7
X G X X X

Attempt 2/6: 2+2=4
X G Y G Y

Attempt 3/6: 4+1=5
Y G X G G

Attempt 4/6: 1+4=5
G G G G G

Correct! You solved it in 4 attempt(s).
```

In this example, the hidden answer was `1+4=5`. Invalid inputs (e.g., syntactically broken or mathematically incorrect equations) are rejected without consuming an attempt.

**Implementation Notes**:

- The implementation is written in F# targeting .NET 10.
- The game runs as a console application with no GUI and no external library dependencies.
- The answer equation is randomly selected at program start using `System.Random`.
- All user interaction occurs via standard input (`stdin`) and standard output (`stdout`).
