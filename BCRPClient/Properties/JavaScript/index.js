require('./fingerpoint.js');

mp.events.add("RAGE::Eval", (code) => eval(code));