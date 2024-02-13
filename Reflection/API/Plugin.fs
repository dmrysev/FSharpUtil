namespace Util.API.Reflection

type IPlugin =
    abstract Load<'a> : unit -> 'a seq
