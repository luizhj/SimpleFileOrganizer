open System.IO

type SilentOption = SilentOutput | NotSilentOutput

type CommandLineOptions = 
    { 
        silent : SilentOption
        paths : array<string> 
        help : bool
    } 

let defaultOptions = {
    silent = NotSilentOutput;
    paths = [] |> Array.ofList ;
    help = false;
    }

type FileData = 
    {
        Year:string 
        Month:string 
        Name:string 
        FullName:string
    }

type MyFile = 
    {
        Path:string
        FileName:string
        FullName:string
    }

type ToMove = 
    {
        OldFile:string
        NewFile:string
    }

let movefile (file:string,newfile:string) = 
    printfn "Moving file %s to %s" file newfile
    try 
        File.Move(file,newfile)
    with
        | ex -> printfn "Error moving file %s :'%s'" file ex.Message

let changearray (path:string,files:array<FileInfo>) =
    files |> Array.map(fun (x:FileInfo) -> ({ Year = x.CreationTime.Year.ToString()
                                              Month = x.CreationTime.Month.ToString().PadLeft(2,'0')
                                              Name = x.Name
                                              FullName = x.FullName
                                            }))
          |> Array.map(fun (x:FileData) -> ( { Path = Path.Combine( [| path; x.Year; x.Month |] )
                                               FileName = x.Name
                                               FullName = x.FullName
                                             }) )

let isHiddenOrSystem filename = 
    let att = File.GetAttributes(filename).ToString()
    not (att.Contains("Hidden") || att.Contains("System")) 

let startprocess(path:string) =

    let executable = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName

    if (Directory.Exists(path)) then 
        let dir = new DirectoryInfo(path)
        let files = dir.GetFiles() 
                        |> Array.filter(fun (x:FileInfo) -> (x.FullName <> executable))
                        |> Array.filter(fun (x:FileInfo) -> (isHiddenOrSystem x.FullName))

        if (files.Length > 0) then
            changearray(path,files)
                            |> Array.map(fun (x:MyFile) -> (x.Path))
                            |> Array.groupBy(fun x -> (x))
                            |> Array.map(fun x -> (Directory.CreateDirectory(fst(x))))
                            |> ignore

            changearray(path,files)
                        |> Array.map(fun (x:MyFile) -> ({ OldFile = x.FullName
                                                          NewFile = Path.Combine(x.Path,x.FileName)
                                                        }))
                        |> Array.map(fun x -> ( movefile(x.OldFile,x.NewFile)))
                        |> ignore

        else
            printfn "Not found files to organize in folder %s." path
    else 
        printfn "Directory not found. %s" path


let start (path:string) =
    if (path.Contains("\"")) then
        startprocess(path.Replace("\"",""))
    else
        startprocess(path)

let rec parseCommandLine args optionsSoFar = 
    match args with 

    // empty list means we're done.
    | [] -> 
        optionsSoFar  

    // match silent
    | "/s"::xs -> 
        let newOptionsSoFar = { optionsSoFar with silent = SilentOption.SilentOutput}
        parseCommandLine xs newOptionsSoFar 

    // match silent
    | "/?"::xs -> 
        let newOptionsSoFar = { optionsSoFar with help = true}
        parseCommandLine xs newOptionsSoFar 

    // handle unrecognized option and keep looping
    | x::xs -> 
        let array = [x] |> Array.ofList
        let lista = Array.append array optionsSoFar.paths 
        let newOptionsSoFar = { optionsSoFar with paths = lista}
        parseCommandLine xs newOptionsSoFar 

let printHelp (print:bool) =
    if print then
        printfn "Usage: FileOrganizer.exe <[path path]> </s> </?>"
        printfn @"   [path path] list of paths: separated with spaces, without \ at the end and inside of quote marks"
        printfn """     Sample: "c:\folder with space" e:\temp "\\remote\temp\another folder"  """
        printfn "       Optional - Default value current folder"
        printfn ""
        printfn "   /s - Silent: doesn't ask to press return to exit"
        printfn "       Optional - Default value ask for return to exit"
        printfn ""
        printfn "   /? - Help: show this message "

[<EntryPoint>]
let main argv =
    let param = argv |> List.ofArray
    let result = parseCommandLine param defaultOptions

    // se mostr ajuda sai
    if (result.help) then
        printHelp true
    else
        // se passou caminhos
        if (result.paths.Length > 0) then
            // loop nos caminhos
            result.paths |> Array.map(fun x -> start(x)) |> ignore
        // se nao passou
        else
            // usa a pasta atual
            let path = Directory.GetCurrentDirectory()
            start(path) |> ignore

        // se nao deve ser silencioso
        if (result.silent = SilentOption.NotSilentOutput) then
            printfn "Press return key to finish."
            stdin.ReadLine() |> ignore
    0 // return an integer exit code

