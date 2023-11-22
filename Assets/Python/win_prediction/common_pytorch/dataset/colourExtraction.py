import os
import cv2
import numpy as np
import matplotlib.pyplot as plt
from sklearn.cluster import KMeans

from collections import Counter 
from sklearn.cluster import KMeans 
from matplotlib import colors 
import matplotlib.pyplot as plt 
import numpy as np 
import cv2

img = cv2.imread("C:/Users/warre/Desktop/thesis_code/win_prediction/results/test6/result_rectify.jpg")
#img = cv2.GaussianBlur(img, (5,5),0)
img_og = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
img = img_og[35:120, 193:245]
print(img.shape)

#mask = np.zeros(img.shape[:2], dtype="uint8")
#cv2.rectangle(mask,(5,5),(img.shape[1]-5, img.shape[0]-5), 255, -1)
#img = cv2.bitwise_and(img, img, mask=cv2.bitwise_not(mask))



#gray = cv2.Canny(img,10,40)
plt.imshow(img)
plt.show()
position = [193 , 115]
size = [52, 75]
# converting image into grayscale image
#gray = cv2.cvtColor(img, cv2.COLOR_RGB2GRAY)
sharpen_kernel = np.array([[-1,-1,-1], [-1,9,-1], [-1,-1,-1]])
gray = cv2.filter2D(img, -1, sharpen_kernel)
plt.imshow(img)
plt.show()

thresh = cv2.threshold(gray, 160, 255, cv2.THRESH_BINARY_INV)[1]
kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (3,3))
close = cv2.morphologyEx(thresh, cv2.MORPH_CLOSE, kernel, iterations=2)


#ret,thresh = cv2.threshold(gray, 125,255,0)
contours,hierarchy = cv2.findContours(close, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
print("Number of contours detected:", len(contours))

for cnt in contours:
   x1,y1 = cnt[0][0]
   approx = cv2.approxPolyDP(cnt, 0.01*cv2.arcLength(cnt, True), True)
   if len(approx) == 4:
      x, y, w, h = cv2.boundingRect(cnt)
      ratio = float(w)/h
      if ratio >= 0.9 and ratio <= 1.1:
         img = cv2.drawContours(img, [cnt], -1, (0,255,255), 3)
         cv2.putText(img, 'Square', (x1, y1), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 0), 2)
      else:
         cv2.putText(img, 'Rectangle', (x1, y1), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)
         img = cv2.drawContours(img, [cnt], -1, (0,255,0), 3)

plt.imshow(img)
plt.show()

def remove_window(img_og, position, size, windowList):
  mask = np.zeros(img_og.shape[:2], dtype="uint8")
  for i in range(len(windowList)):
    cv2.rectangle(mask, ((np.array(etageList[i]['position'])[0, 0].copy()),(np.array(etageList[i]['position'])[0, 1].copy())), 
                  ((np.array(etageList[i]['position'])[2, 0].copy()),(np.array(etageList[i]['position'])[2, 1].copy())), 255, -1)  
  masked = cv2.bitwise_and(img_og, img_og, mask=cv2.bitwise_not(mask))
  plt.imshow(masked)
  plt.show()



def filter_neutral_colors(img_og, lower = (0, 0, 0), upper = (500, 0,255)):
  ## convert to hsv
  hsv = cv2.cvtColor(img_og, cv2.COLOR_BGR2HSV)
  plt.imshow(hsv)
  plt.show()
  ## mask of gray (0,0,0) ~ (0, 0,255)
  mask = cv2.inRange(hsv, lower, upper)
  plt.imshow(mask)
  plt.show()
  ## filter out gray colours (convert them to white, which will be excluded at the end)
  img_filtered = cv2.bitwise_and(img_og,img_og, mask=cv2.bitwise_not(mask))
  print("Original sum check: ",np.sum(img_og))
  print("Filtered sum check: ",np.sum(img_filtered))
  return img_filtered



def clamp(x): 
  return int(max(0, min(x, 255)))

def rgb_to_hex(rgb_color):
  hex_color = "#"
  for i in rgb_color:
    hex_color += ("{:02x}".format(int(i)))
  return hex_color


def extract_dominant_colors(img, k_clusters = 4):
  
  img_og = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
  
  img_filtered = filter_neutral_colors(img_og, lower = (0, 0, 0), upper = (50, 0, 0))

  plt.imshow(img_filtered)
  plt.show()
  
  img = cv2.cvtColor(img_filtered, cv2.COLOR_BGR2RGB)
  plt.imshow(img_filtered)
  plt.show()
  
  img = img.reshape((img.shape[0] * img.shape[1],3)) #represent as row*column,channel number
  clt = KMeans(n_clusters=k_clusters) #cluster number
 
  color_labels = clt.fit_predict(img)
  print(color_labels)
  center_colors = clt.cluster_centers_
  counts = Counter(color_labels)
  ordered_colors = [center_colors[i] for i in counts.keys()]
  ordered_colors = np.flip(ordered_colors)
  hex_colors = [rgb_to_hex(ordered_colors[i]) for i in counts.keys()]

  plt.figure(figsize = (12, 8))
  plt.pie(counts.values(), labels = hex_colors, colors = hex_colors)

  plt.show()

  for color in hex_colors:
    print(color)
  


def analyze(img):
  img_filtered = filter_neutral_colors(img, lower = (0, 0, 0), upper = (180, 255, 110))

  plt.imshow(img_filtered)
  plt.show()

  img_filtered = img_filtered.reshape((img_filtered.shape[0] * img_filtered.shape[1],3)) #represent as row*column,channel number
  clf = KMeans(n_clusters = 3)
  color_labels = clf.fit_predict(img_filtered)
  center_colors = clf.cluster_centers_
  labels = clf.labels_
  label_l, count = np.unique(labels, return_counts= True)
  counts = Counter(color_labels)
  ordered_colors=[]
  values = []
  for k in sorted(counts, key=counts.get, reverse= True):
        print(center_colors[k])
        ordered_colors.append(center_colors[k])
        print(counts[k])
        values.append(counts[k])
  
  for i in range(len(ordered_colors)):
    print(ordered_colors[i])
 
  hex_colors = [rgb_to_hex(ordered_colors[i]) for i in range(len(ordered_colors))]

  plt.figure(figsize = (12, 8))
  plt.pie(values, labels = hex_colors, colors = hex_colors)

  plt.show()

  #plt.savefig("results/my_pie.png")
  print("Found the following colors:\n")
  for color in hex_colors:
    print(color)


analyze(img)


