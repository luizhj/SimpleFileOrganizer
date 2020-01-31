open System.IO

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
    
[<EntryPoint>]
let main argv =

    if (argv.Length > 0) then
        argv |> Array.map(fun x -> (start(x)))
        |> ignore

    else
        let path = Directory.GetCurrentDirectory()
        start(path) 
        |> ignore

    printfn "Press return key to finish."
    stdin.ReadLine() |> ignore
    0 // return an integer exit code

