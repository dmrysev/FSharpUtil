module Util.Reactive

open System
open System.ComponentModel

type ObservableValue<'a>(observable: IObservable<'a>, getValue: unit -> 'a) as this =
    let ev = new Event<_,_>()
    do
        observable.Add(fun v -> ev.Trigger(this, PropertyChangedEventArgs("Value")))
    member this.Value with get () = getValue()
    member val Changed = observable
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = ev.Publish

type NotifyValue<'a>(initialState: 'a) =
    let ev = new Event<_,_>()
    let valueChanged = Event<'a>()
    let mutable value = initialState
    member this.Value 
        with get () = value
        and set (v) =
            value <- v
            ev.Trigger(this, PropertyChangedEventArgs("Value"))
            valueChanged.Trigger value
    member val Changed = valueChanged.Publish
    member this.AsObservableValue() =
        ObservableValue(this.Changed, fun _ -> value)
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = ev.Publish
