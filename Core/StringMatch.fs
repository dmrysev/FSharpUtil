module Util.StringMatch

let isImageFile text = text |> Util.Regex.isMatchIgnoreCase @"^.+\.(png|jpg|jpeg|webp)$"
let isVideoFile text = text |> Util.Regex.isMatchIgnoreCase @"^.+\.(webm|flv|vob|ogg|ogv|drc|gifv|mng|avi|mov|qt|wmv|yuv|rm|rmvb|asf|amv|mp4|m4v|mp*|m?v|svi|3gp|flv|f4v|gif|mpeg|mpg|mkv|ts)$"
let isComicBookFile text = text |> Util.Regex.isMatchIgnoreCase @"^.+\.(cbz|cbr|cb7)$"
let notEmpty text = text <> ""

let isInteger text =
    text |> Util.Regex.isMatch @"\d+"

let isUrl (text: string) =
    text |> String.startsWith "http://" || text |> String.startsWith "https://"
