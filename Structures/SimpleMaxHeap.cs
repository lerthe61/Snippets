// Will throw once the limit is exceeded
public class SimpleMaxHeap:
{
    private int[] _heap;
    private long _size = 0;

    public SimpleMaxHeap(long capacity) {
        _heap = new int[capacity];        
    }

    private long GetParentIndex(long index) {
        return (index - 1) / 2;        
    }

    private long GetLeftChildIndex(long index) {
        return 2 * index + 1;
    }

    private long GetRightChildIndex(long index) {
        return 2 * (index + 1);
    }

    // Swap two nodes
    private void Swap(long index1, long index2) {
        var temp = _heap[index1];
        _heap[index1] = _heap[index2];
        _heap[index2] = temp;
    }
    
    // bubbling up element until it takes his place
    private void SiftUp(long index) {
        // Moving up while element at index is bigger than it's parent element        
        while (index > 1 && (_heap[index] > _heap[GetParentIndex(index)])) {
            var parentIndex = GetParentIndex(index);
            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    // sinking element at index down, choosing child with biggest value
    private void SiftDown(long index) {
        var maxIndex = index;
        var leftChildIndex = GetLeftChildIndex(index);
        if (leftChildIndex < _size && _heap[leftChildIndex] > _heap[maxIndex]) {
            maxIndex = leftChildIndex;
        }
        var rightChildIndex = GetRightChildIndex(index);
        if (rightChildIndex < _size && _heap[rightChildIndex] > _heap[maxIndex]) {
            maxIndex = rightChildIndex;
        }

        // condition met, no reason to continue
        if (index == maxIndex) return;
        Swap(index, maxIndex);
        SiftDown(maxIndex);
    }

    public int Peek() {
        return _heap[0];
    }

    public int Pop() {
        var top = _heap[0];
        _heap[0] = _heap[_size - 1];
        _size--;
        SiftDown(0);
        return top;
    }

    public void Push(int elem) {
        if (_size == _heap.LongLength)
            throw new Exception("Limit is exceeded");
        _size++;
        _heap[_size - 1] = elem;
        SiftUp(_size - 1);
    }
}