const observeHiddenRefResizedCallback = (event) => {
  const [elt] = event
  const { height, width, left, top } = elt.contentRect
  DotNet.invokeMethodAsync('TheArchives.Client', 'HiddenRefResized', height, width, left, top)
}

const observeReaderRefResizedCallback = (event) => {
  const [elt] = event
  const { height, width, left, top } = elt.contentRect
  DotNet.invokeMethodAsync('TheArchives.Client', 'ReaderRefResized', height, width, left, top)
}

const bodyClass = 'overflow-h'
const hiddenRefResizeObserver = new ResizeObserver(observeHiddenRefResizedCallback)
const readerRefResizeObserver = new ResizeObserver(observeReaderRefResizedCallback)

window.interopFunctions = window.interopFunctions || {}
Object.assign(window.interopFunctions, {
  getBoundingClientRect: (element) => element.getBoundingClientRect(),
  observeHiddenRefResized: (element) => {
    hiddenRefResizeObserver.observe(element)
  },
  unObserveHiddenRefResized: (element) => {
    hiddenRefResizeObserver.unobserve(element)
  },
  observeReaderRefResized: (element) => {
    readerRefResizeObserver.observe(element)
  },
  unObserveReaderRefResized: (element) => {
    readerRefResizeObserver.unobserve(element)
  },
  initializeBody: () => {
    if (!document.body.classList.contains(bodyClass)) {
      document.body.classList.add(bodyClass)
    }
  },
  disposeBody: () => {
    if (document.body.classList.contains(bodyClass)) {
      document.body.classList.remove(bodyClass)
    }
  },
})
