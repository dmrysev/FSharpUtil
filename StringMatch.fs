module Util.StringMatch

let isImageFile text =
    text |> Util.Regex.isMatch @".+\.(png|jpg|jpeg)"

let notEmpty text = text <> ""

let isInteger text =
    text |> Util.Regex.isMatch @"\d+"
