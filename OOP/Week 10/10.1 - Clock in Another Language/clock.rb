class Counter
  attr_accessor :name
  def initialize(name, ticks = 0)
    @count = ticks
    @name = name
  end

  def ticks
    @count
  end

  def increment
    @count += 1
  end

  def reset
    @count = 0
  end
end

class Clock
  def initialize(time = nil)
    @secs = Counter.new("secs")
    @mins = Counter.new("mins")
    @hours = Counter.new("hours")

    if time != nil
      set_from_time(time)
      return
    end
  end

  def set_from_time(time)
    segments = time.split(':')
    raise "Invalid time format" if segments.size > 3
    if segments.size == 3
      @hours = Counter.new("hours", segments[0].to_i)
    end
    if segments.size >= 2
      @mins = Counter.new("mins", segments[-2].to_i)
    end
    if segments.size >= 1
      @secs = Counter.new("secs", segments[-1].to_i)
    end
  end

  def tick
    @secs.increment
    if @secs.ticks >= 60
      @secs.reset
      @mins.increment
    end
    if @mins.ticks >= 60
      @mins.reset
      @hours.increment
    end
    if @hours.ticks >= 24
      @hours.reset
    end
  end

  def reset
    @secs.reset
    @mins.reset
    @hours.reset
  end

  def to_s
    "%02d:%02d:%02d" % [@hours.ticks, @mins.ticks, @secs.ticks]
  end
end

def main
  clock = Clock.new("23:59:59")
  clock.tick
  puts clock
end

if __FILE__ == $0
  main
end
