open System.IO

let movefile (file:string,newfile:string) = 
    stdout.WriteLine("Moving file "+file+" to "+newfile)
    try 
        File.Move(file,newfile)
    with
        | ex -> stdout.WriteLine("Error moving file "+file+" '"+ex.Message+"'")

let changearray (path:string,files:array<FileInfo>) =
    files |> Array.map(fun x -> ([|x.CreationTime.Year.ToString();x.CreationTime.Month.ToString(); x.Name; x.FullName|]))
          |> Array.map(fun x -> ([|x.[0];x.[1].PadLeft(2,'0');x.[2];x.[3] |] ))
          |> Array.map(fun x -> ([|Path.Combine([|path;x.[0];x.[1]|]);x.[2];x.[3]   |]) )

let isHiddenOrSystem filename = 
    let att = File.GetAttributes(filename).ToString()
    not (att.Contains("Hidden") || att.Contains("System")) 

let startprocess(path:string) =

    let executable = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName

    if (Directory.Exists(path)) then 
        let dir = new DirectoryInfo(path)
        let files = dir.GetFiles() 
                        |> Array.filter(fun x -> (x.FullName <> executable))
                        |> Array.filter(fun x -> (isHiddenOrSystem x.FullName))

        if (files.Length > 0) then
            changearray(path,files)
                            |> Array.map(fun x -> (x.[0]))
                            |> Array.groupBy(fun x -> (x))
                            |> Array.map(fun x -> (Directory.CreateDirectory(fst(x))))
                            |> ignore

            changearray(path,files)
                        |> Array.map(fun x -> ([| Path.Combine(x.[0],x.[1]); x.[2]  |]))
                        |> Array.map(fun x -> ( movefile(x.[1],x.[0])))
                        |> ignore

            0 |> ignore
        else
            stdout.WriteLine("Not found files to organize in folder "+path+".")
            0 |> ignore
    else 
        stdout.WriteLine("Directory not found. "+path)
        0 |> ignore 


let start (path:string) =
    if (path.Contains("\"")) then
        startprocess(path.Replace("\"",""))
        0 |> ignore
    else
        startprocess(path)
        0 |> ignore
    0
    
[<EntryPoint>]
let main argv =

    if (argv.Length > 0) then
        argv |> Array.map(fun x -> (start(x)))
             |> ignore
        0 |>ignore
    else
        let path = Directory.GetCurrentDirectory()
        start(path) |> ignore
        0 |>ignore

    stdout.WriteLine("Press return key to finish.")
    stdin.ReadLine() |> ignore
    0 // return an integer exit code

