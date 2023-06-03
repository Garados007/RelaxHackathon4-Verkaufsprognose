#!/usr/bin/env Rscript

# maybe need to install the packaged:
# install.packages("data.table")
# install.packages("ggplot2")
# install.packages("plotrix")

# maybe need to load the packages:
# library("ggplot2")
library("data.table") # used for easy filtering of the data tables
library("plotrix") # used for violin plot

# reading data (asumes working dir contains sales_task_padded)
# sales_task_padded is the same as sales_task with padded 0 values when an article isn't sold at a day
sales<-read.csv("sales_task_padded.csv")
# converting the read data to data table
sales = data.table(sales)

filterById = function(id) {
    sales[product_id %in% id]
}

salesById = function(id) {
    filterById(id)$product_count
}

# function for padding a vector to a certain length by insterting 0
# padTo = function (vector, len) {
#     vector_len = length(vector)
#     pad_len = len - vector_len 
#     if (pad_len <= 0) {
#         return(vector)
#     }
#     padding = rep(0, pad_len)
#     return(c(vector, padding))
# }
# not used so it's commented out but kept here to allow recreating the padding

meanById = function(id) {
    mean(salesById(id))
}

# histogram of all sales:
hist(sales$product_count, breaks=29) # hardcoded breaks, should be set to force "width" of buckets to be 1 in clean analysis
# looks like two normal distributions

# mean of sales per ID, where ID is (0 indexed) position in the vector
list_of_sales_means = sapply(X=0:99, FUN=meanById)

# I did some violin plots to check the distributions here are a few examples:
violin_plot(list_of_sales_means, col="grey")
violin_plot(salesById(0), col="grey")
violin_plot(salesById(27), col="grey")
violin_plot(salesById(42), col="grey")
violin_plot(salesById(99), col="grey")

# I also checked the demand over time, and it looked like it wasn't changing that much and roughly normal distributed around 2 points (maybe 3, the first distribution beeing more dominant than the second one and the thrid one beein only bairly noticeable)

# note quite some other plots and checks were omited here
# conclusion: the approach to approximate the sales over time as periodic functions using fourier coefficients (only the real part) would not work very well and should be dropped
