﻿package;
public class Issue2285_10 {
	public function new() {
		var m:Issue2285_10;
		switch(m.foo()) {
			case Foo: 
			case Bar:
		}
	}
	
	function foo(v):EFoo2285_10 {
		return Bar;
	}
}

@:enum abstract EFoo2285_10(Int) {
	var Foo = 0;
	var Bar = 1;
}