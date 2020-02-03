module Util.StringMatch

open System.Text.RegularExpressions

let isImageFile text =
    let regex = Regex @".+\.(png|jpg|jpeg)"
    regex.IsMatch text