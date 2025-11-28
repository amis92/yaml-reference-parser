M := ../../.cache/makes
$(shell [ -d $M ] || git clone -q https://github.com/makeplus/makes $M)
include $M/init.mk
include $M/node.mk
include $M/clean.mk
include $M/shell.mk

ROOT := $(shell cd ../..; pwd)
BASE := $(ROOT)/parser-1.2
PARSER-LANG := javascript
BIN := node
GRAMMAR :=

ALL-JS := \
  bin/yaml-parser \
  lib/grammar.js \
  lib/parser.js \
  lib/prelude.js \
  lib/receiver.js \
  lib/test-receiver.js \
  test/testml-bridge.js \

BUILD-DEPS := \
  $(ROOT)/node_modules \
  build-coffee \
  $(ALL-JS) \
  $(ROOT)/test/testml/src/node/lib \

include $(ROOT)/.common.mk

yaml-parser: $(ROOT)/node_modules
	./bin/yaml-parser

build-coffee:
	$(MAKE) -C $(BASE)/coffeescript build

bin/%: $(BASE)/coffeescript/bin/%
	echo '#!/usr/bin/env node' > $@
	coffee -cp $< >> $@
	chmod +x $@

lib/%.js: $(BASE)/coffeescript/lib/%.coffee
	coffee -cp $< > $@

test/%.js: $(BASE)/coffeescript/test/%.coffee
	coffee -cp $< > $@
	perl -pi -e 's{/coffee/}{/node/}' $@

$(ROOT)/test/testml/src/node/lib: $(ROOT)/test/testml
	$(MAKE) -C $(@:%/lib=%) js-files
