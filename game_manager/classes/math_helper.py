def map_range(value, from_min, from_max, to_min, to_max):
    # First, normalize the input value to a 0-1 range within the source range
    normalized_value = (value - from_min) / (from_max - from_min)

    # Then, map the normalized value to the target range
    mapped_value = normalized_value * (to_max - to_min) + to_min

    return mapped_value