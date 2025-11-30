(ns yaml-parser.core
  (:require [yaml-parser.parser :as p]
            [yaml-parser.receiver :as r]
            [yaml-parser.test-receiver :as tr]
            [yaml-parser.grammar :as g]))

(defn parse-yaml
  "Parse a YAML string and return events."
  [yaml-str]
  (let [receiver (tr/make-test-receiver)
        parser (p/make-parser receiver)]
    (p/parse parser yaml-str)
    (tr/output receiver)))

(defn -main [& args]
  (let [input (slurp *in*)]
    (println (parse-yaml input))))
