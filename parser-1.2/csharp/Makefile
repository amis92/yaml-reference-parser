ROOT := $(shell cd ../..; pwd)
BASE := $(ROOT)/parser-1.2
PARSER-LANG := csharp

GRAMMAR := lib/Grammar.cs
SPEC-PATCHED-YAML := $(BASE)/build/yaml-spec-1.2-patched.yaml
SPEC-YAML := $(BASE)/build/yaml-spec-1.2.yaml
SPEC-PATCH := $(BASE)/build/yaml-spec-1.2.patch

.DELETE_ON_ERROR:

default:: build

build:
	dotnet build --configuration Release --nologo -v q

test:: build $(ROOT)/test/suite
	dotnet run --configuration Release --no-build -- --test-suite $(ROOT)/test/suite/src

$(GRAMMAR): $(SPEC-PATCHED-YAML) generate_grammar.cs
	cd $(BASE) && dotnet run $(PARSER-LANG)/generate_grammar.cs -- \
		--from=$(SPEC-PATCHED-YAML) --rule=l-yaml-stream \
		> $(PARSER-LANG)/$(GRAMMAR)

generate: $(GRAMMAR)

$(SPEC-PATCHED-YAML): $(SPEC-YAML)
	cp $< $@
	patch $@ < $(SPEC-PATCH)

$(SPEC-YAML):
	cp $(ROOT)/../yaml-grammar/yaml-spec-1.2.yaml $@ || \
	wget -O $@ https://raw.githubusercontent.com/yaml/yaml-grammar/master/yaml-spec-1.2.yaml || \
	curl -o $@ https://raw.githubusercontent.com/yaml/yaml-grammar/master/yaml-spec-1.2.yaml

clean::
	dotnet clean --nologo -v q 2>/dev/null || true
	rm -rf bin obj

$(ROOT)/test/suite:
	mkdir -p $@
	git -C $@ init --quiet
	git -C $@ remote add origin https://github.com/yaml/yaml-test-suite
	git -C $@ fetch origin main
	git -C $@ reset --hard FETCH_HEAD
