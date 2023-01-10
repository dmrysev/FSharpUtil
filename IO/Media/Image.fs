namespace Util.IO.Media.Image

open Util.IO.Path

module File =
    type OpenOptions = { Title: string option }
    with static member Default = { Title = None }

    let openProcess (options: OpenOptions) (imageFilePath: FilePath) =
        let fileName = imageFilePath |> FilePath.fileName
        let title = options.Title |> Option.defaultValue fileName.Value
        let imageProccess = 
            new System.Diagnostics.Process() 
            |> Util.Process.useBashScript $"feh --title='{title}' -ZFYr '{imageFilePath.Value}'"
            |> Util.Process.noRedirect
        imageProccess.StartInfo.UseShellExecute <- false        
        imageProccess

    let closeProcessByTitle title =
        Util.Process.executeNoOutput $"wmctrl -c {title}"
