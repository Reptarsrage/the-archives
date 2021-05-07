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

const handleTouchStart = (event) => {
  const { clientX, clientY } = event.touches[0]
  DotNet.invokeMethodAsync('TheArchives.Client', 'ReaderRefTouchStart', clientX, clientY)
}

const handleTouchMove = (event) => {
  const { clientX, clientY } = event.touches[0]
  DotNet.invokeMethodAsync('TheArchives.Client', 'ReaderRefTouchMove', clientX, clientY)
}

const bodyClass = 'overflow-h'
const hiddenRefResizeObserver = new ResizeObserver(observeHiddenRefResizedCallback)
const readerRefResizeObserver = new ResizeObserver(observeReaderRefResizedCallback)

window.interopFunctions = window.interopFunctions || {}
Object.assign(window.interopFunctions, {
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
  listenToSwipes() {
    document.addEventListener('touchstart', handleTouchStart, false)
    document.addEventListener('touchmove', handleTouchMove, false)
  },
  unlistenToSwipes() {
    document.removeEventListener('touchstart', handleTouchStart)
    document.removeEventListener('touchmove', handleTouchMove)
  },
})
