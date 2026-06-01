# Mathle

A console-based math equation guessing game for CS20200. Guess the hidden equation — think Wordle, but with arithmetic. Choose a difficulty stage before each game.

## How to Run

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```
dotnet run
```

Run from the repository root (the directory containing `Mathle.fsproj`).

## Rules

The hidden equation has the form `A op B = C`, where A and B are single-digit integers and op is one of `+`, `-`, `*`, `/`. You have 6 attempts to guess it.

After each valid guess, you receive one symbol per token:

| Symbol | Meaning |
|--------|---------|
| `G` | Correct token in the correct position |
| `Y` | Token exists in the equation but in a different position |
| `X` | Token does not appear in the equation |

Both compact (`3+4=7`) and space-separated (`3 + 4 = 7`) input formats are accepted. Syntactically invalid guesses and mathematically incorrect equations (e.g., `2+2=5`) are rejected without using up an attempt.

### Stages

| Stage  | Operators       | Attempts |
|--------|-----------------|----------|
| Easy   | `+` and `-` only | 6       |
| Normal | All (`+` `-` `*` `/`) | 6  |
| Hard   | All (`+` `-` `*` `/`) | 5  |

### Feedback colors

Each feedback symbol is displayed in a colored cell (requires ANSI-compatible terminal):

| Symbol | Color  | Meaning |
|--------|--------|---------|
| `G`    | Green  | Correct position |
| `Y`    | Yellow | Wrong position |
| `X`    | Gray   | Not in equation |

## Changes from Proposal

The following requirements were **added** after the initial proposal submission. All original requirements (1–11) remain unchanged.

- **Requirements 12–16 (Stage selection):** A stage selection screen was added before the game begins, offering Easy (+ and − only, 6 tries), Normal (all operators, 6 tries, identical to original proposal), and Hard (all operators, 5 tries). Added to improve replayability and provide graduated difficulty.
- **Requirement 17 (Play again):** After each game ends, the player is asked whether to play again. If yes, the game returns to the stage selection screen. Added to avoid having to relaunch the program between games.
- **Feedback display (Implementation Notes):** Feedback symbols G/Y/X are shown in ANSI color-coded cells alongside the guessed token values, displayed in a Wordle-style two-line grid (token row + feedback row per attempt). The G/Y/X symbols required by requirements 2 and 8 are preserved; the change is a visual enhancement only.

## LLM Usage

I used Claude (claude-sonnet-4-6 via Claude Code) to assist with implementing this project.

**What I used it for:** I used Claude to write the core F# implementation based on the game design and requirements I had specified — specifically the equation pool generation, input parser, and the Wordle-style feedback algorithm for handling duplicate tokens.

**What I had to manually change or reprompt:** The first draft of the feedback function used a mutable boolean array to track which answer tokens had been matched, which was imperative in style. I reprompted asking for a rewrite using `List.fold` with an immutable remaining-token pool, which is more consistent with the functional approach expected in this course. I also had to clarify that the subtraction case should exclude zero and negative results from the answer pool, since the initial version included `A - B = 0` equations.

**What the LLM was not able to do correctly:** The example interaction in `requirements.md` contains feedback results that are inconsistent with the algorithm described in Requirement 9. Claude initially tried to reconcile the two by producing a different algorithm, but no single algorithm can satisfy both simultaneously. I had to explicitly direct it to implement Requirement 9 as stated (G first, then Y left-to-right against unmatched answer tokens), treating the example interaction as illustrative rather than authoritative.
