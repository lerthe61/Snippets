To quickly switch to a different namespace, you can set up the following alias:
```alias kcd='kubectl config set-context $(kubectl config current-context) --namespace '```. You can then switch between namespaces using ```kcd some-namespace```.