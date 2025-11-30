M := ../../.cache/makes
$(shell [ -d $M ] || git clone -q https://github.com/makeplus/makes $M)
include $M/init.mk
include $M/babashka.mk
include $M/node.mk
include $M/clean.mk
include $M/shell.mk

ROOT := $(shell cd ../..; pwd)
BASE := $(ROOT)/parser-1.2

SPEC-PATCHED-YAML := $(BASE)/build/yaml-spec-1.2-patched.yaml
SPEC-YAML := $(BASE)/build/yaml-spec-1.2.yaml
SPEC-PATCH := $(BASE)/build/yaml-spec-1.2.patch

GRAMMAR-CLJ := src/yaml_parser/grammar.clj

MAKES-CLEAN := \
  $(GRAMMAR-CLJ) \
  .cpcache \

.DELETE_ON_ERROR:

default:: build

build:: $(BB) $(GRAMMAR-CLJ)

test:: build $(ROOT)/test/suite
	prove -v test/*.t

clean::

$(GRAMMAR-CLJ): $(BB) $(SPEC-PATCHED-YAML) bin/generate-yaml-grammar
	$(BB) bin/generate-yaml-grammar \
	  --from=$(SPEC-PATCHED-YAML) \
	  --rule=l-yaml-stream \
	  > $@

$(SPEC-PATCHED-YAML): $(SPEC-YAML)
	cp $< $@
	patch $@ < $(SPEC-PATCH)

$(SPEC-YAML):
	cp $(ROOT)/../yaml-grammar/yaml-spec-1.2.yaml $@ || \
	wget https://raw.githubusercontent.com/yaml/yaml-grammar/master/yaml-spec-1.2.yaml || \
	curl -O https://raw.githubusercontent.com/yaml/yaml-grammar/master/yaml-spec-1.2.yaml

# Clone test suite if needed
$(ROOT)/test/suite:
	mkdir -p $@
	git -C $@ init --quiet
	git -C $@ remote add origin https://github.com/yaml/yaml-test-suite
	git -C $@ fetch origin main
	git -C $@ reset --hard FETCH_HEAD
