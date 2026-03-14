ROOT := $(shell cd ../..; pwd)
BASE := $(ROOT)/parser-1.2
PARSER-LANG := csharp

GRAMMAR := lib/Grammar.cs

.PHONY: test build clean generate

build:
	dotnet build --configuration Release --nologo -v q

test:: build $(ROOT)/test/suite
	dotnet run --configuration Release --no-build -- --test-suite $(ROOT)/test/suite/src

generate:
	cd $(BASE) && dotnet run $(PARSER-LANG)/generate_grammar.cs -- \
		perl/lib/Grammar.pm $(PARSER-LANG)/$(GRAMMAR)

clean::
	dotnet clean --nologo -v q 2>/dev/null || true
	rm -rf bin obj

$(ROOT)/test/suite:
	mkdir -p $@
	git -C $@ init --quiet
	git -C $@ remote add origin https://github.com/yaml/yaml-test-suite
	git -C $@ fetch origin main
	git -C $@ reset --hard FETCH_HEAD
