import math
import tensorflow as tf, keras 
import matplotlib.pyplot as plt
import numpy as np

# Determine loss based on outputs from real and fake images. Outputs on real
# images are compared against a vector of ones (the actual labels) and outputs
# on fake images are compared against a vector of zeroes.
crossEntropyFn = tf.keras.losses.BinaryCrossentropy(from_logits=True)

def discriminatorLoss(real, fake):
  ones = tf.ones_like(real)      # vector of ones with the same size as real
  zeros = tf.zeros_like(fake)    # vector of zeroes with the same size as fake

  realLoss = crossEntropyFn(ones, real)
  fakeLoss = crossEntropyFn(zeros, fake)
  return realLoss + fakeLoss

# Determine loss based on the output of the discriminator on fake images.
# Here the output is compared against a vector of all ones, corresponding to
# the ideal case where the discriminator is completel fooled on all fakes.
def generatorLoss(fake):
  ones = tf.ones_like(fake)
  fakeLoss = crossEntropyFn(ones, fake)
  return fakeLoss

from datetime import datetime
import pytz

# These can be changed to improve training. In particular, more epochs and 
# greater datasetSize will lead to a better trained model at the cost of time.
epochs = 20
batchSize = 128
noiseSize = 100
datasetSize = 60000

# get training data
(trainData, trainLabels) = keras.datasets.mnist.load_data()[0]
assert datasetSize <= trainData.shape[0]

# # reduce dataset size
dataset = trainData[:datasetSize]

# normalize pixel values
dataset = (dataset - 127.5) / 127.5

# return batchSize dataset images for training
def getBatch(num=batchSize):
  # create batch from a random set of indices
	randIdxs = np.random.randint(0, dataset.shape[0], num)
	batch = dataset[randIdxs]

  # add an extra traning column and return
	return np.expand_dims(batch, axis=-1)

# use one batch to train the generator and discriminator
def trainBatch(realImgs):
  # These tapes will help us compute gradients for the optimizer. They 'watch'
  # what happens when the models run and use the resulting losses to update
  # model parameters.
  with tf.GradientTape() as genTape, tf.GradientTape() as dscTape:
    # make generator fakes
    fakeImgs = generator(tf.random.normal((batchSize, noiseSize)))

    # run the discriminator on the batch of real and fake images
    realOutput = discriminator(realImgs, training=True)
    fakeOutput = discriminator(fakeImgs, training=True)

    # calculate losses
    dscLoss = discriminatorLoss(realOutput, fakeOutput)
    genLoss = generatorLoss(fakeOutput)

    # get gradients from tape as a list of changes
    dscGradients = dscTape.gradient(dscLoss, discriminator.trainable_variables)
    genGradients = genTape.gradient(genLoss, generator.trainable_variables)

    # modify neural network weights and parameters based on gradient
    dscChanges = zip(dscGradients, discriminator.trainable_variables)
    genChanges = zip(genGradients, generator.trainable_variables)
    dscOptimizer.apply_gradients(dscChanges)
    genOptimizer.apply_gradients(genChanges)

def train():
  # train over the entire dataset for the number of epochs
  for epoch in range(epochs):
    # print start time
    cmuTime = datetime.now(pytz.timezone("America/New_York"))
    t = cmuTime.strftime("%H:%M:%S")
    print(f"Starting Epoch {epoch} at time {t}")

    # run through the dataset in random batches
    for batch in range(datasetSize // batchSize):
      trainBatch(getBatch())
    
    # Every 5 epochs, save the models 
    if (epoch + 1) % 5 == 0:
      generator.save("generator" + str(epoch) + ".h5")
      discriminator.save("discriminator" + str(epoch) + ".h5")
      print(f"Saved models at epoch {epoch}!")

generator = keras.models.load_model("./generator24.h5")
discriminator = keras.models.load_model("./discriminator24.h5")

train()
