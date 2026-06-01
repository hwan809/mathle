open System

// ── Types ─────────────────────────────────────────────────────────────────

type Op    = Add | Sub | Mul | Div
type Token = Num of int | Oper of Op | Eq
type Stage = Easy | Normal | Hard

type Equation    = { A: int; Op: Op; B: int; C: int }
type StageConfig = { Label: string; Desc: string; Pool: Equation list; MaxTries: int }

// ── ANSI ──────────────────────────────────────────────────────────────────

module Ansi =
    let reset  = "\x1b[0m"
    let green  = "\x1b[42m\x1b[30m"
    let yellow = "\x1b[43m\x1b[30m"
    let gray   = "\x1b[100m\x1b[97m"
    let colorOf = function 'G' -> green | 'Y' -> yellow | _ -> gray

// ── Domain ────────────────────────────────────────────────────────────────

let opChar = function Add -> '+' | Sub -> '-' | Mul -> '*' | Div -> '/'

let charToOp = function
    | '+' -> Some Add | '-' -> Some Sub | '*' -> Some Mul | '/' -> Some Div
    | _ -> None

let eval op a b =
    match op with
    | Add -> Some (a + b)
    | Sub -> let r = a - b in if r > 0 then Some r else None
    | Mul -> Some (a * b)
    | Div -> if b <> 0 && a % b = 0 then Some (a / b) else None

let allEquations =
    [ for a in 1..9 do
        for b in 1..9 do
          for op in [Add; Sub; Mul; Div] do
            match eval op a b with
            | Some c -> yield { A = a; Op = op; B = b; C = c }
            | None   -> () ]

let configOf = function
    | Easy   -> { Label = "Easy";   Desc = "+ and - only, 6 tries"
                  Pool = allEquations |> List.filter (fun e -> e.Op = Add || e.Op = Sub)
                  MaxTries = 6 }
    | Normal -> { Label = "Normal"; Desc = "all operators, 6 tries"
                  Pool = allEquations; MaxTries = 6 }
    | Hard   -> { Label = "Hard";   Desc = "all operators, 5 tries"
                  Pool = allEquations; MaxTries = 5 }

// ── Tokenization & Parsing ────────────────────────────────────────────────

let toTokens eq  = [| Num eq.A; Oper eq.Op; Num eq.B; Eq; Num eq.C |]
let tokenStr     = function Num n -> string n | Oper o -> string (opChar o) | Eq -> "="

let parseGuess (raw: string) =
    let s     = raw.Replace(" ", "")
    let opIdx = s |> Seq.tryFindIndex (fun c -> c = '+' || c = '-' || c = '*' || c = '/')
    let eqIdx = s.LastIndexOf '='
    match opIdx with
    | Some i when eqIdx > i ->
        match Int32.TryParse s.[..i-1], charToOp s.[i],
              Int32.TryParse s.[i+1..eqIdx-1], Int32.TryParse s.[eqIdx+1..] with
        | (true, a), Some op, (true, b), (true, c)
            when a >= 1 && a <= 9 && b >= 1 && b <= 9 && c >= 1 ->
            Some [| Num a; Oper op; Num b; Eq; Num c |]
        | _ -> None
    | _ -> None

let isValid = function
    | [| Num a; Oper op; Num b; Eq; Num c |] -> eval op a b = Some c
    | _ -> false

// ── Feedback ─────────────────────────────────────────────────────────────

let feedback (answer: Token[]) (guess: Token[]) =
    let isG  = Array.init 5 (fun i -> guess.[i] = answer.[i])
    let pool = [0..4] |> List.choose (fun i -> if isG.[i] then None else Some answer.[i])
    [0..4]
    |> List.fold (fun (rem, acc) i ->
        if isG.[i] then rem, acc @ ['G']
        else
            match rem |> List.tryFindIndex ((=) guess.[i]) with
            | Some j -> List.removeAt j rem, acc @ ['Y']
            | None   -> rem, acc @ ['X']
    ) (pool, [])
    |> snd |> Array.ofList

// ── Rendering ────────────────────────────────────────────────────────────

let clearScreen () = try Console.Clear() with _ -> ()

// Each cell is 5 visible chars wide: " XX  " for tokens, " [G] " for feedback.
let renderGuessRow (tokens: Token[]) (fbs: char[]) =
    tokens
    |> Array.map (fun t -> sprintf " %2s  " (tokenStr t))
    |> String.concat "" |> printfn "  %s"

    fbs
    |> Array.map (fun fb -> sprintf " %s[%c]%s " (Ansi.colorOf fb) fb Ansi.reset)
    |> String.concat "" |> printfn "  %s"

    printfn ""

let renderEmptyRow () =
    printfn "  %s\n" (String.replicate 5 "  _  ")

let renderBoard (cfg: StageConfig) (history: (Token[] * char[]) list) =
    clearScreen ()
    printfn "\n  ╔═══════════════════════╗"
    printfn "  ║     M A T H L E       ║"
    printfn "  ╚═══════════════════════╝"
    printfn "  %s — %s" cfg.Label cfg.Desc
    printfn "  Guess the hidden equation (A op B = C) in %d tries." cfg.MaxTries
    printfn "  Feedback: G = right position, Y = wrong position, X = not present\n"
    history |> List.iter (fun (t, fb) -> renderGuessRow t fb)
    for _ in 1..(cfg.MaxTries - List.length history) do renderEmptyRow ()

// ── Input ─────────────────────────────────────────────────────────────────

let rec getGuess () =
    printf "  > "
    let line = Console.ReadLine()
    if isNull line then None
    else
        match parseGuess line with
        | None ->
            printfn "  Invalid input. Enter an equation like 3+4=7 or 3 + 4 = 7."
            getGuess ()
        | Some guess when not (isValid guess) ->
            printfn "  The equation is incorrect (e.g. 2+2 is not 5). Try again."
            getGuess ()
        | Some guess -> Some guess

// ── Game Loop ─────────────────────────────────────────────────────────────

let rec play (cfg: StageConfig) (answer: Equation) (history: (Token[] * char[]) list) attempt =
    renderBoard cfg history
    printfn "  Attempt %d/%d\n" attempt cfg.MaxTries
    match getGuess () with
    | None -> ()
    | Some guess ->
        let fb       = feedback (toTokens answer) guess
        let history' = history @ [guess, fb]
        if Array.forall ((=) 'G') fb then
            renderBoard cfg history'
            printfn "\n  Correct! You solved it in %d attempt(s)." attempt
        elif attempt >= cfg.MaxTries then
            renderBoard cfg history'
            printfn "\n  Game over. The answer was: %d%c%d=%d"
                answer.A (opChar answer.Op) answer.B answer.C
        else
            play cfg answer history' (attempt + 1)

// ── Stage Selection ───────────────────────────────────────────────────────

let selectStage () =
    clearScreen ()
    printfn "\n  ╔═══════════════════════╗"
    printfn "  ║     M A T H L E       ║"
    printfn "  ╚═══════════════════════╝\n"
    printfn "  Select a stage:\n"
    printfn "  [1]  Easy    —  + and - only, 6 tries"
    printfn "  [2]  Normal  —  all operators, 6 tries"
    printfn "  [3]  Hard    —  all operators, 5 tries\n"
    let rec loop () =
        printf "  > "
        match Console.ReadLine() with
        | "1" -> Easy
        | "2" -> Normal
        | "3" -> Hard
        | _   -> printfn "  Please enter 1, 2, or 3."; loop ()
    loop ()

// ── Entry Point ───────────────────────────────────────────────────────────

[<EntryPoint>]
let main _ =
    Console.OutputEncoding <- System.Text.Encoding.UTF8
    let rng = Random()
    let rec loop () =
        let stage  = selectStage ()
        let cfg    = configOf stage
        let answer = cfg.Pool.[rng.Next cfg.Pool.Length]
        play cfg answer [] 1
        let rec askReplay () =
            printf "\n  Play again? (y/n): "
            match Console.ReadLine() with
            | "y" | "Y" -> loop ()
            | "n" | "N" -> ()
            | null      -> ()
            | _         -> printfn "  Please enter y or n."; askReplay ()
        askReplay ()
    loop ()
    0
