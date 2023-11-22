import matplotlib.pyplot as plt
import numpy as np
import os
from PIL import Image


img = np.asarray(Image.open(r'C:\Users\carlw\Thesis\Thesistext\cornerselect_640.png'))
#imgplot = plt.imshow(img,origin='upper')
reversed_image_data = np.flipud(img)
imgplot = plt.imshow(reversed_image_data,origin='upper')
plt.axis([0, 640, 0, 640])
plt.xlabel("x")
plt.ylabel("y")

plt.show()
