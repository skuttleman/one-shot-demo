(ns one-shot.core
  (:require
    [babashka.process :as p]))

(def ^:const ^:private SYNC_PUSH_COMMAND
  "aws s3 sync Assets/Resources s3://one-shot")

(def ^:const ^:private SYNC_PULL_COMMAND
  "aws s3 sync s3://one-shot Assets/Resources")

(defn ^:private sh [command]
  (p/process ["sh" "-c" command]))

(defn -main [& args]
  (case (vec args)
    ["s3" "push"] (sh SYNC_PUSH_COMMAND)
    ["s3" "pull"] (sh SYNC_PULL_COMMAND)
    ["fuck" "yourself"] (println "... I die a little every time. \uD83D\uDE14")))
