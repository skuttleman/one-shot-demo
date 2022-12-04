(ns one-shot.core
  (:require
    [babashka.process :as p]))

(def ^:const ^:private SYNC_PUSH_COMMAND
  "aws s3 cp Assets/Resources s3://one-shot/ --recursive --exclude \"*\" --include \"*.png\" --include \"*.xcf\"")

(def ^:const ^:private SYNC_PULL_COMMAND
  "aws s3 cp s3://one-shot/ Assets/Resources --recursive --exclude \"*\" --include \"*.png\" --include \"*.xcf\"")

(defn ^:private sh [command]
  (p/process ["sh" "-c" command]))

(defn -main [& args]
  (case (vec args)
    ["s3" "push"] (sh SYNC_PUSH_COMMAND)
    ["s3" "pull"] (sh SYNC_PULL_COMMAND)
    ["fuck" "yourself"] (println "... I die a little every time. \uD83D\uDE14")))
