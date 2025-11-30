(ns testml-bridge
  (:require [yaml-parser.parser :as p]
            [yaml-parser.test-receiver :as tr]
            [clojure.string :as str]))

;; TestML Bridge for Clojure YAML parser

(defn parse
  "Parse YAML and return events or error."
  ([yaml] (parse yaml nil))
  ([yaml expect-error]
   (let [receiver (tr/make-test-receiver)
         parser (p/make-parser receiver)]
     (try
       (p/parse parser yaml)
       (if expect-error
         0
         (tr/output receiver))
       (catch Exception e
         (if expect-error
           1
           (str e)))))))

(defn unescape
  "Unescape test input text."
  [text]
  (-> text
      (str/replace "␣" " ")
      (str/replace #"—*»" "\t")
      (str/replace "⇔" "\uFEFF")
      (str/replace "↵" "")
      (str/replace #"∎\n$" "")))

(defn fix-test-output
  "Fix test output (identity for now)."
  [text]
  text)
