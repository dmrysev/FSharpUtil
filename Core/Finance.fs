module Util.Finance

type Currency =
| EUR
| USD

module Measure =
    [<Measure>] type EUR
    [<Measure>] type USD
